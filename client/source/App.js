import React, { useState } from "react";
import ReactDOM from 'react-dom';

import './App.scss';

import { Welcome, Home } from './components/';

const App = () => {
    //const test = 'logging';
    const test = 'logged';
    const [stage, setStage] = useState(test);

    let content = <Welcome onLogin={_ => setStage('logged')}></Welcome>;
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