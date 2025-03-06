import React, { useEffect, useState } from 'react';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';
import { Divider } from '@mui/material';
import Panel from './Panel';
import ConfigDialog from './ConfigDialog';
import config from '../config.json';

// This function loads the remote component, GPT generated code
const loadComponent = async (scope: string, module: string) => {
  await __webpack_init_sharing__('default');
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const container = (window as any)[scope]; // Get remote container
  await container.init(__webpack_share_scopes__.default);
  const factory = await container.get(`./${module}`);
  return factory();
};

const TabPanel = () => {
  const [tabValue, setTabValue] = useState(0);
  const tabLabels = config.mfes.map((mfe) => mfe.componentLabel);
  const [components, setComponents] = useState<React.FC[]>([]);

  useEffect(() => {
    const imports: React.FC[] = [];

    // Lazy load the components
    config.mfes.forEach((mfe) => {
      const remote = mfe.name;
      const module = mfe.component;
      try {
        const component = React.lazy(() => loadComponent(remote, module));
        imports.push(component);
      } catch (error) {
        console.error(
          `Error loading component from ${remote}/${module}:`,
          error,
        );
      }
    });

    setComponents(imports);
  }, []);

  const handleChange = (_: React.SyntheticEvent, newTabValue: number) =>
    setTabValue(newTabValue);

  return (
    <React.Fragment>
      <Box sx={{ width: '100%' }}>
        <Box>
          <Tabs
            value={tabValue}
            onChange={handleChange}
            aria-label="tabs example"
            centered
          >
            {tabLabels.map((label, idx) => (
              <Tab label={label} key={idx} />
            ))}
          </Tabs>
        </Box>
        <Divider variant="middle" />
        {components.map((Component, idx) => (
          <Panel value={tabValue} index={idx} key={idx}>
            <React.Suspense fallback={<div>Loading...</div>}>
              <Component />
            </React.Suspense>
          </Panel>
        ))}
      </Box>
      <Divider variant="middle" />
      <Box padding={2}>
        <ConfigDialog />
      </Box>
    </React.Fragment>
  );
};

export default TabPanel;
