import { JsonForms } from '@jsonforms/react';
import {
  materialCells,
  materialRenderers,
} from '@jsonforms/material-renderers';
import { Button, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useEffect, useMemo, useState } from 'react';
import muiDataGridRenderer from './MaterialDataGridControlRenderer';

const API_URL = 'http://localhost:8081/api';

const Form = () => {
  const [data, setData] = useState({});
  const [schema, setSchema] = useState({});
  const [uiSchema, setUiSchema] = useState({ type: '' });
  const [errors, setErrors] = useState<object[] | undefined>([]);
  const [apiResponse, setApiResponse] = useState<string | undefined>();
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

  const handleSubmit = async () => {
    console.log('Submit', data);
    const response = await fetch(`${API_URL}/event`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      console.log('Error response from server event handler');
      setApiResponse('Error response from server event handler');
      return;
    }

    const response_json = await response.json();
    setData(response_json);
    setApiResponse(JSON.stringify(response_json, null, 2));
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
        <Typography variant={'h6'}>API Response</Typography>
        <pre>{apiResponse}</pre>
      </Grid>
    </Grid>
  );
};

export default Form;
