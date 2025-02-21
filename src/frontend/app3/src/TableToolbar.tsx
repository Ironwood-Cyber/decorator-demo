/* eslint-disable @typescript-eslint/no-explicit-any */
import React from 'react';
import {
  ArrayTranslations,
  ControlElement,
  createDefaultValue,
  JsonSchema,
} from '@jsonforms/core';
import {
  Badge,
  FormHelperText,
  Grid2 as Grid,
  IconButton,
  styled,
  Tooltip,
  Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';

const StyledBadge = styled(Badge)(({ theme }: any) => ({
  color: theme.palette.error.main,
}));

export interface MaterialTableToolbarProps {
  numColumns: number;
  errors: string;
  label: string;
  description: string;
  path: string;
  uischema: ControlElement;
  schema: JsonSchema;
  rootSchema: JsonSchema;
  enabled: boolean;
  translations: ArrayTranslations;
  addItem(path: string, value: any): () => void;
  disableAdd?: boolean;
  data: number;
}

const TableToolbar = React.memo(function TableToolbar({
  errors,
  label,
  description,
  path,
  addItem,
  schema,
  enabled,
  translations,
  rootSchema,
  disableAdd,
  data,
}: MaterialTableToolbarProps) {
  return (
    <React.Fragment>
      <Grid
        container
        alignItems="center"
        justifyContent="space-between"
        spacing={2}
      >
        <Grid
          container
          spacing={4}
          sx={{ display: 'flex', alignItems: 'center' }}
        >
          <Typography variant={'h6'}>{label}</Typography>
          {errors.length !== 0 && (
            <Tooltip id="tooltip-validation" title={errors}>
              <StyledBadge badgeContent={errors.split('\n').length}>
                <ErrorOutlineIcon color="inherit" />
              </StyledBadge>
            </Tooltip>
          )}
        </Grid>

        {enabled && !disableAdd ? (
          <Tooltip
            id="tooltip-add"
            title={translations.addTooltip}
            placement="bottom"
          >
            <IconButton
              aria-label={translations.addAriaLabel}
              onClick={addItem(path, {
                ...createDefaultValue(schema, rootSchema),
                id: `${data + 1}`,
              })}
              size="large"
            >
              <AddIcon />
            </IconButton>
          </Tooltip>
        ) : null}
      </Grid>
      {description && <FormHelperText>{description}</FormHelperText>}
    </React.Fragment>
  );
});

export default TableToolbar;
