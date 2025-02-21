import { person } from '@jsonforms/examples';
import { JsonForms } from '@jsonforms/react';
import {
  materialCells,
  materialRenderers,
} from '@jsonforms/material-renderers';
import { Button, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useMemo, useState } from 'react';

const schema = person.schema;
const uiSchema = person.uischema;
const initialData = person.data;

const Form = () => {
  const [data, setData] = useState(initialData);
  const [errors, setErrors] = useState<object[] | undefined>([]);
  const stringifiedData = useMemo(() => JSON.stringify(data, null, 2), [data]);

  const handleSubmit = () => {
    console.log('Submit', data);
  };

  console.log('FORM 2 RENDERING');

  return (
    <Grid container spacing={2} padding={2}>
      <Grid size={12}>
        <Typography variant={'h5'}>Form Two</Typography>
      </Grid>
      <Grid size={8}>
        <JsonForms
          schema={schema}
          uischema={uiSchema}
          data={data}
          renderers={materialRenderers}
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
