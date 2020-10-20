import { hot } from 'react-hot-loader/root';
import React, { useState } from "react";
import { BrowserRouter, Route, Switch } from 'react-router-dom';

import { Welcome, Home } from './pages';

import './App.scss';

try {
    var is_in_dev = require('../fake_data/fake.json');
} catch (err) { }

const App = () => (
    <BrowserRouter>
        <div className="app">
            <div className="app__content">
                <Route exact path="/">
                    <Welcome></Welcome>
                </Route>
                <Route path="/signup">
                    <Welcome></Welcome>
                </Route>
                <Route path="/chat">
                    <Home></Home>
                </Route>
            </div>
        </div>
    </BrowserRouter>
);

export default hot(App);
