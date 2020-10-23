import { hot } from 'react-hot-loader/root';
import React, { useState } from "react";
import { BrowserRouter, Route, Switch } from 'react-router-dom';

import { ROUTES } from 'shared';

import { Welcome, Home } from './pages';

import './App.scss';

try {
    var is_in_dev = require('../fake_data/fake.json');
} catch (err) { }

const App = () => (
    <BrowserRouter>
        <div className="app">
            <div className="app__content">
                <Switch>
                    <Route path={ROUTES.Chat.route}>
                        <Home></Home>
                    </Route>
                    <Route path="/">
                        <Welcome></Welcome>
                    </Route>
                </Switch>
            </div>
        </div>
    </BrowserRouter>
);

export default hot(App);
