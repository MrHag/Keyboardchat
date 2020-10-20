import React from 'react';
import { Route } from 'react-router-dom';

import SignIn from './layouts/SignIn/SignIn';
import SignUp from './layouts/SignUp/SignUp';

import './Welcome.scss';

const Welcome = () => (
    <div className="welcome">
        <Route exact path="/">
            <SignIn />
        </Route>
        <Route exact path="/signup">
            <SignUp></SignUp>
        </Route>
    </div>
);

export default Welcome;