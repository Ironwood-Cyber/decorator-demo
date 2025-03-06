import { JsonForms } from '@jsonforms/react';
import {
  materialCells,
  materialRenderers,
} from '@jsonforms/material-renderers';
import { Button, Typography } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useEffect, useMemo, useState } from 'react';

// Need to run backend/app1-backend/gateway-service for this to work

const API_URL = 'http://localhost:8080/api';

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
    // Try to fetch the local event handler first, if it fails send the payload to the server
    let response = await fetch(`${API_URL}/event`);
    if (!response.ok) {
      console.log(
        'Error response from fetching local event handler, sending to server for processing...',
      );

      response = await fetch(`${API_URL}/event`, {
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
      return;
    }

    // Otherwise, use the local event handler
    const response_json = await response.json();
    const jsCode = response_json['handler'];
    const handler = new Function(`return ${jsCode}`)();

    // For some reason if i directly use the result of handler(payload) the state doesn't update properly
    // so i'm spreading it into a new object which seems to work
    const result = { ...handler(data) };
    setData(result);
    setApiResponse(JSON.stringify(result, null, 2));
  };

  console.log('FORM 1 RENDERING');

  return (
    <Grid container spacing={2} padding={2}>
      <Grid size={12}>
        <Typography variant={'h5'}>Form One</Typography>
      </Grid>
      <Grid size={8}>
        {Object.keys(schema).length !== 0 && (
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
        )}
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
