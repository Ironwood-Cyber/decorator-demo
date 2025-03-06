import { JsonForms } from '@jsonforms/react';
import {
  materialCells,
  materialRenderers,
} from '@jsonforms/material-renderers';
import { Button, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useEffect, useMemo, useState } from 'react';
import muiDataGridRenderer from './MaterialDataGridControlRenderer';

// Need to run backend/app3-backend/gateway-service for this to work

const API_URL = 'http://localhost:8081/api';

const Form = () => {
  const [data, setData] = useState({});
  const [schema, setSchema] = useState({});
  const [uiSchema, setUiSchema] = useState({ type: '' });
  const [errors, setErrors] = useState<object[] | undefined>([]);
  const stringifiedData = useMemo(() => JSON.stringify(data, null, 2), [data]);

  useEffect(() => {
    fetch(`${API_URL}/data`)
      .then((response) => response.json())
      .then((response_json) => {
        setData(response_json);
      })
      .catch((err) => {
        console.warn(err.message);
      });

    fetch(`${API_URL}/schema`)
      .then((response) => response.json())
      .then((response_json) => {
        setSchema(response_json);
      })
      .catch((err) => {
        console.warn(err.message);
      });

    fetch(`${API_URL}/uischema`)
      .then((response) => response.json())
      .then((response_json) => {
        setUiSchema(response_json);
      })
      .catch((err) => {
        console.warn(err.message);
      });
  }, []);

  const handleSubmit = () => {
    console.log('Submit', data);
  };

  console.log('FORM 3 RENDERING');

  // Add the custom MUI DataGrid renderer to the list of renderers
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
