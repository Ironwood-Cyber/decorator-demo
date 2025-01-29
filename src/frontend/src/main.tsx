import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import {
  Container,
  createTheme,
  CssBaseline,
  ThemeProvider,
} from '@mui/material';

const theme = createTheme({
  components: {
    MuiFormControl: {
      styleOverrides: {
        root: {
          margin: '0.8em 0',
        },
      },
    },
  },
});

const rootEl = document.getElementById('root');

if (!rootEl) throw new Error('Failed to find root element');

createRoot(rootEl).render(
  <StrictMode>
    <ThemeProvider theme={theme}>
      <Container maxWidth="xl">
        <CssBaseline />
        <App />
      </Container>
    </ThemeProvider>
  </StrictMode>,
);
