import React from 'react';
import { ErrorObject } from 'ajv';

import {
  ArrayLayoutProps,
  ArrayTranslations,
  ControlElement,
  encode,
  errorAt,
  formatErrorMessage,
  getData,
  JsonFormsCellRendererRegistryEntry,
  JsonFormsCore,
  JsonFormsRendererRegistryEntry,
  JsonSchema,
  Paths,
  Resolve,
} from '@jsonforms/core';
import { WithDeleteDialogSupport } from '@jsonforms/material-renderers';
import {
  DispatchCell,
  JsonFormsStateContext,
  useJsonForms,
} from '@jsonforms/react';

import { startCase, merge, isEmpty, union } from 'lodash';

import { Box, FormHelperText, Typography } from '@mui/material';
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import {
  DataGrid,
  GridActionsCellItem,
  GridColDef,
  GridRenderCellParams,
  GridToolbar,
} from '@mui/x-data-grid';

import TableToolbar from './TableToolbar';

//////////////////////////////////// UTIL ////////////////////////////////////
const generateCell = (
  Cell: React.ComponentType<OwnPropsOfNonEmptyCell>,
  schema: JsonSchema,
  rowPath: string,
  enabled: boolean,
  prop: string,
  cells?: JsonFormsCellRendererRegistryEntry[],
): React.ReactElement => {
  if (schema.type === 'object') {
    const cellPath = Paths.compose(rowPath, prop);
    const props = {
      propName: prop,
      schema,
      title: schema.properties?.[prop]?.title ?? startCase(prop),
      rowPath,
      cellPath,
      enabled,
      cells,
    };
    return <Cell key={cellPath} {...props} />;
  } else {
    // primitives
    const props = {
      schema,
      rowPath,
      cellPath: rowPath,
      enabled,
    };
    return <Cell key={rowPath} {...props} />;
  }
};

const getValidColumnProps = (scopedSchema: JsonSchema): string[] => {
  if (
    scopedSchema.type === 'object' &&
    typeof scopedSchema.properties === 'object'
  ) {
    return Object.keys(scopedSchema.properties).filter((prop) => {
      return scopedSchema.properties![prop].type !== 'array';
    });
  }
  // primitives
  return [''];
};

const controlWithoutLabel = (scope: string): ControlElement => ({
  type: 'Control',
  scope: scope,
  label: false,
});
/////////////////////////////////////////////////////////////////////////////

const EmptyTable = () => {
  return (
    <Box
      display="flex"
      flexDirection="column"
      justifyContent="center"
      alignItems="center"
      sx={{ height: '100%' }}
    >
      <Typography variant="h6" color="text.secondary" sx={{ p: 2 }}>
        No data
      </Typography>
    </Box>
  );
};

interface NonEmptyCellComponentProps {
  path: string;
  propName?: string;
  schema: JsonSchema;
  rootSchema: JsonSchema;
  errors: string;
  enabled: boolean;
  renderers?: JsonFormsRendererRegistryEntry[];
  cells?: JsonFormsCellRendererRegistryEntry[];
  isValid: boolean;
}
const NonEmptyCellComponent = React.memo(function NonEmptyCellComponent({
  path,
  propName,
  schema,
  rootSchema,
  errors,
  enabled,
  renderers,
  cells,
  isValid,
}: NonEmptyCellComponentProps) {
  return (
    <React.Fragment>
      {schema.properties ? (
        <DispatchCell
          schema={Resolve.schema(
            schema,
            `#/properties/${encode(propName as string)}`,
            rootSchema,
          )}
          uischema={controlWithoutLabel(
            `#/properties/${encode(propName as string)}`,
          )}
          path={path}
          enabled={enabled}
          renderers={renderers}
          cells={cells}
        />
      ) : (
        <DispatchCell
          schema={schema}
          uischema={controlWithoutLabel('#')}
          path={path}
          enabled={enabled}
          renderers={renderers}
          cells={cells}
        />
      )}
      <FormHelperText error={!isValid}>{!isValid && errors}</FormHelperText>
    </React.Fragment>
  );
});

interface NonEmptyCellProps extends OwnPropsOfNonEmptyCell {
  rootSchema: JsonSchema;
  errors: string;
  path: string;
  enabled: boolean;
}
interface OwnPropsOfNonEmptyCell {
  rowPath: string;
  propName?: string;
  schema: JsonSchema;
  enabled: boolean;
  renderers?: JsonFormsRendererRegistryEntry[];
  cells?: JsonFormsCellRendererRegistryEntry[];
}
const ctxToNonEmptyCellProps = (
  ctx: JsonFormsStateContext,
  ownProps: OwnPropsOfNonEmptyCell,
): NonEmptyCellProps => {
  const path =
    ownProps.rowPath +
    (ownProps.schema.type === 'object' ? '.' + ownProps.propName : '');
  const errors = formatErrorMessage(
    union(
      errorAt(
        path,
        ownProps.schema,
      )(ctx.core as JsonFormsCore).map(
        (error: ErrorObject) => error.message,
      ) as string[],
    ),
  );
  return {
    rowPath: ownProps.rowPath,
    propName: ownProps.propName,
    schema: ownProps.schema,
    rootSchema: (ctx.core as JsonFormsCore).schema,
    errors,
    path,
    enabled: ownProps.enabled,
    cells: ownProps.cells || ctx.cells,
    renderers: ownProps.renderers || ctx.renderers,
  };
};

const NonEmptyCell = (ownProps: OwnPropsOfNonEmptyCell): React.ReactElement => {
  const ctx = useJsonForms();
  const emptyCellProps = ctxToNonEmptyCellProps(ctx, ownProps);
  const isValid = isEmpty(emptyCellProps.errors);

  return <NonEmptyCellComponent {...emptyCellProps} isValid={isValid} />;
};

export const MaterialDataGridControl = (
  props: ArrayLayoutProps &
    WithDeleteDialogSupport & { translations: ArrayTranslations },
) => {
  const {
    label,
    description,
    path,
    schema,
    rootSchema,
    uischema,
    errors,
    openDeleteDialog,
    visible,
    enabled,
    cells,
    translations,
    disableAdd,
    disableRemove,
    config,
    data,
  } = props;

  const addItem = (path: string, value: unknown) => props.addItem!(path, value);

  const appliedUiSchemaOptions = merge({}, config, uischema.options);
  const doDisableAdd = disableAdd || appliedUiSchemaOptions.disableAdd;
  const doDisableRemove = disableRemove || appliedUiSchemaOptions.disableRemove;

  const ctx = useJsonForms();
  console.log('CTX', ctx);
  const state = { jsonforms: ctx };
  console.log('STATE', state);
  const theData = getData(state);
  console.log('DATA', theData);

  const controlElement = uischema as ControlElement;
  const isObjectSchema = schema.type === 'object';
  const headerCells: GridColDef[] | undefined = isObjectSchema
    ? (getValidColumnProps(schema).map((prop) => {
        return {
          key: prop,
          field: prop,
          headerName: startCase(prop),
          flex: 1,
          resizable: true,
          // valueGetter: (_, row) => ({ id: row.id }),
          renderCell: (params: GridRenderCellParams) => {
            const childPath = Paths.compose(
              path,
              `${(params.id as number) - 1}`,
            );
            return generateCell(
              NonEmptyCell,
              schema,
              childPath,
              enabled,
              prop,
              cells,
            );
          },
        };
      }) as GridColDef[])
    : undefined;

  // Add actions column if remove is enabled
  if (!doDisableRemove) {
    headerCells!.push({
      field: 'actions',
      type: 'actions',
      headerName: 'Actions',
      width: 100,
      cellClassName: 'actions',
      getActions: ({ id }: { id: number }) => [
        <GridActionsCellItem
          key="Delete"
          icon={<DeleteIcon />}
          label="Actions"
          onClick={() =>
            openDeleteDialog(Paths.compose(path, `${id - 1}`), id - 1)
          }
          color="inherit"
        />,
      ],
    } as GridColDef);
  }

  if (!visible) {
    return null;
  }

  return (
    <div
      style={{
        height: 'auto',
        width: '100%',
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <TableToolbar
        errors={errors}
        label={label}
        description={description as string}
        addItem={addItem}
        numColumns={isObjectSchema ? headerCells!.length : 1}
        path={path}
        uischema={controlElement}
        schema={schema}
        rootSchema={rootSchema}
        enabled={enabled}
        translations={translations}
        disableAdd={doDisableAdd}
        data={data}
      />
      <DataGrid
        getRowHeight={() => 'auto'}
        rows={theData.myArray}
        columns={headerCells as GridColDef[]}
        slots={{ noRowsOverlay: EmptyTable, toolbar: GridToolbar }}
        initialState={{
          columns: {
            columnVisibilityModel: {
              id: false,
            },
          },
        }}
      />
    </div>
  );
};
