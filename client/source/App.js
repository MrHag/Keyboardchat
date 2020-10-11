import React, { useState } from "react";
import ReactDOM from 'react-dom';

import './App.scss';

import { Welcome, Home } from './components/';

try {
    var is_in_dev = require('../fake_data/fake.json');
} catch (err) { }

const App = () => {
    const [stage, setStage] = useState('logging');

    let content = <Welcome onLogin={_ => setStage('logged')}></Welcome>;
    if (is_in_dev !== undefined) {
        //content = <Home></Home>;
    }

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