import React from "react";
import TabPanel from "./TabPanel";
import { Container, CssBaseline } from "@mui/material";

const App = () => {
  return (
    <React.Fragment>
      <CssBaseline />
      <Container fixed sx={{ padding: 4 }}>
        <TabPanel />
      </Container>
    </React.Fragment>
  );
};

export default App;
