import { hot } from 'react-hot-loader/root';
import React, { useState } from "react";

import { Welcome, Home } from './components/';
import './App.scss';

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

export default hot(App);