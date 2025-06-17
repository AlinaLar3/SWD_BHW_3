import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';

import Aside from './Components/UI/Aside';
import './styles/App.css';

function App() {
  const content = (
    <>
      <div className='page'>
        <Routes>
          <Route path='/transactions' element={'fgdfgdf'} />

          <Route path='/accounts' element={'gfdgfdsgdfsg'} />
        </Routes>
      </div>
    </>
  );

  return (
    <Router>
      <Aside>{content}</Aside>
    </Router>
  );
}

export default App;
