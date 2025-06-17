import { configureStore } from '@reduxjs/toolkit';
import themeSlice from '../features/themeSlice';

const store = configureStore({
  reducer: {
    theme: themeSlice,
  },
});

export default store;
