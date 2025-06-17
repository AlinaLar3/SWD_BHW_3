import { createRoot } from 'react-dom/client';
import store from './store/store';
import { Provider } from 'react-redux';
import ThemeWrapper from './ThemeWrapper.jsx';

import './styles/index.css';
import './styles/themes.scss';

createRoot(document.getElementById('root')).render(
  <Provider store={store}>
    <ThemeWrapper />
  </Provider>,
);
