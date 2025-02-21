import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Divider,
} from '@mui/material';
import React, { useState } from 'react';
import config from '../config.json';

const ConfigDialog = () => {
  const [dialogOpen, setDialogOpen] = useState(false);

  return (
    <React.Fragment>
      <Button variant="contained" onClick={() => setDialogOpen(true)}>
        Show Config
      </Button>
      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        maxWidth="lg"
      >
        <DialogTitle>Config</DialogTitle>
        <Divider variant="middle" />
        <DialogContent>
          <DialogContentText padding={2}>
            <pre>{JSON.stringify(config, null, 2)}</pre>
          </DialogContentText>
        </DialogContent>
        <Divider variant="middle" />
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </React.Fragment>
  );
};

export default ConfigDialog;
