import { useEffect, useMemo, useState } from 'react';
import { JsonForms } from '@jsonforms/react';
import {
  materialCells,
  materialRenderers,
} from '@jsonforms/material-renderers';
import { Grid2, Typography } from '@mui/material';

const API_URL = 'http://localhost:8080/api';

const App = () => {
  const [data, setData] = useState({});
  const [schema, setSchema] = useState({});
  const [uiSchema, setUiSchema] = useState<{ type: string }>({ type: '' });
  const [errors, setErrors] = useState<object[]>([]);
  const [apiResponse, setApiResponse] = useState<string | undefined>();

  const stringifiedData = useMemo(() => JSON.stringify(data, null, 2), [data]);

  // Fetch the JsonForms data, schema and ui schema from the server
  useEffect(() => {
    fetch(`${API_URL}/data`)
      .then(response => response.json())
      .then(response_json => {
        console.debug('Data', response_json);
        setData(response_json);
      })
      .catch(err => {
        console.warn(err.message);
      });

    fetch(`${API_URL}/schema`)
      .then(response => response.json())
      .then(response_json => {
        console.debug('Schema', response_json);
        setSchema(response_json);
      })
      .catch(err => {
        console.warn(err.message);
      });

    fetch(`${API_URL}/uischema`)
      .then(response => response.json())
      .then(response_json => {
        console.debug('UI Schema', response_json);
        setUiSchema(response_json);
      })
      .catch(err => {
        console.warn(err.message);
      });
  }, []);

  const handleChange = async (payload: unknown) => {
    // Prevents infinite loop
    if (JSON.stringify(payload) === JSON.stringify(data)) return;

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
        body: JSON.stringify(payload),
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
    const result = { ...handler(payload) };
    setData(result);
    setApiResponse(JSON.stringify(result, null, 2));
  };

  return (
    <Grid2 container spacing={3} sx={{ padding: '1em 0' }}>
      <Grid2 size={6}>
        <Typography variant={'h4'}>Rendered form</Typography>
        <JsonForms
          schema={schema}
          uischema={uiSchema}
          data={data}
          renderers={materialRenderers}
          cells={materialCells}
          onChange={({ data, errors }) => {
            if (Object.keys(data).length === 0) return;

            errors !== undefined && setErrors(errors);
            errors?.length == 0 && handleChange(data);
          }}
        />
      </Grid2>
      <Grid2 size={6}>
        <Typography variant={'h4'}>Bound data</Typography>
        <div>
          <pre id="boundData">{stringifiedData}</pre>
        </div>
        <Typography variant={'h4'}>API response</Typography>
        <div>
          <pre id="apiResponse">{errors.length == 0 ? apiResponse : ''}</pre>
        </div>
      </Grid2>
    </Grid2>
  );
};

export default App;
