import { useSelector } from 'react-redux';
import { ThemeProvider } from '@gravity-ui/uikit';
import App from './App';

const ThemeWrapper = () => {
  const theme = useSelector((state) => state.theme.theme);

  return (
    <ThemeProvider theme={theme}>
      <App />
    </ThemeProvider>
  );
};
export default ThemeWrapper;
