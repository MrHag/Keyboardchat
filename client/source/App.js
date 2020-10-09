import React, { useState } from "react";
import ReactDOM from 'react-dom';

import './App.scss';

import { Welcome, Home } from './components/';

const App = () => {
    const [stage, setStage] = useState('logging');

    //let content = <Welcome onLogin={_ => setStage('logged')}></Welcome>;
    let content = <Home></Home>;
    if (stage === 'logged') {
        content = <Home></Home>
    }

    return (
        <div className="app">
            <div className="app__content">
                {content}
            </div>
        </div>
    )
}

ReactDOM.render(<App />, document.getElementById('root'));