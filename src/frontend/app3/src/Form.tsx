import { JsonForms } from '@jsonforms/react';
import {
  materialCells,
  materialRenderers,
} from '@jsonforms/material-renderers';
import { Button, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useMemo, useState } from 'react';
import muiDataGridRenderer from './MaterialDataGridControlRenderer';

const schema = {
  type: 'object',
  properties: {
    myArray: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          id: { type: 'string' },
          name: {
            type: 'string',
            minLength: 3,
            description: 'Please enter your name',
          },
          age: { type: 'integer' },
          birthday: {
            type: 'string',
            format: 'date',
          },
          nationality: {
            type: 'string',
            enum: ['DE', 'IT', 'JP', 'US', 'RU', 'Other'],
          },
          USCitizen: {
            type: 'boolean',
          },
        },
        required: ['id', 'name'],
      },
    },
  },
};
const uiSchema = {
  type: 'Control',
  scope: '#/properties/myArray',
};
const initialData = {
  myArray: [
    { id: '1', name: 'Alice', age: 30, date: '' },
    { id: '2', name: 'Bob', age: 25 },
  ],
};

const Form = () => {
  const [data, setData] = useState(initialData);
  const [errors, setErrors] = useState<object[] | undefined>([]);
  const stringifiedData = useMemo(() => JSON.stringify(data, null, 2), [data]);

  const handleSubmit = () => {
    console.log('Submit', data);
  };

  console.log('FORM 3 RENDERING');

  const renderers = [...materialRenderers, muiDataGridRenderer];

  return (
    <Grid container spacing={2} padding={2}>
      <Grid size={12}>
        <Typography variant={'h5'}>Form Three</Typography>
      </Grid>
      <Grid size={8}>
        <JsonForms
          schema={schema}
          uischema={uiSchema}
          data={data}
          renderers={renderers}
          cells={materialCells}
          onChange={({ data, errors }) => {
            setData(data);
            setErrors(errors);
          }}
        />
        <Button
          variant="contained"
          disabled={errors?.length !== 0}
          onClick={() => handleSubmit()}
        >
          Submit
        </Button>
      </Grid>
      <Grid size={4}>
        <Typography variant={'h6'}>Data</Typography>
        <pre>{stringifiedData}</pre>
      </Grid>
    </Grid>
  );
};

export default Form;
