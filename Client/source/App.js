import { hot } from 'react-hot-loader/root';
import React, { useState } from "react";
import { BrowserRouter, Route } from 'react-router-dom';

import { Welcome, Home } from './pages';

import './App.scss';

try {
    var is_in_dev = require('../fake_data/fake.json');
} catch (err) { }

const App = () => (
    <BrowserRouter>
        <div className="app">
            <div className="app__content">
            <Route path="/">
                <Welcome></Welcome>
            </Route>
            <Route path="/home">
                <Home></Home>
            </Route>
            </div>
        </div>
    </BrowserRouter>
);

export default hot(App);
