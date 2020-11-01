import React from 'react';
import { Route, Switch } from 'react-router-dom';

import { ROUTES } from 'shared';

import SignIn from './layouts/SignIn/SignIn';
import SignUp from './layouts/SignUp/SignUp';

import './Welcome.scss';

const Welcome = () => (
  <div className="welcome">
    <Switch>
      <Route path={ROUTES.Signup.route}>
        <SignUp />
      </Route>
      <Route path={ROUTES.Authorization.route}>
        <SignIn />
      </Route>
    </Switch>
  </div>
);

export default Welcome;
