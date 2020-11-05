import React, { useState, useRef, useEffect } from 'react';
import { useHistory, NavLink } from 'react-router-dom';

import { Socket, SocketManager } from 'logic';
import { ROUTES } from 'shared';
import { Button, Input, InputPassword, Form } from 'components';

import './SignIn.scss';

const SignIn = () => {
  const routeHistory = useHistory();

  const [password, setPassword] = useState('');
  const [login, setLogin] = useState('');
  const [err, setErr] = useState('');

  const passwordRef = useRef();

  const onLoginHandler = () => {
    console.log('Socket.emit("auth", ...)');
    const request = {
      name: login,
      password,
    };
    Socket.emit('auth', request);
  };

  const newSocketAuth = (authData) => {
    if (authData.error === null) {
      routeHistory.push(ROUTES.Home.route);
    } else {
      setErr(authData.error);
    }
  };

  useEffect(() => {
    SocketManager.addCallback('auth', newSocketAuth);
    return () => SocketManager.removeCallback('auth', newSocketAuth);
  }, []);

  const onLoginKeyupHandler = (e) => {
    if (e.key === 'Enter') {
      console.log('Login enter up...');
      passwordRef.current.focus();
    }
  };

  const onPasswordKeyupHandler = (e) => {
    if (e.key === 'Enter') {
      onLoginHandler(e);
    }
  };

  return (
    <div className="sign-in">
      <Form
        name="SignIn"
      >
        <Input
          className="sign-in__login-input"
          placeholder="Enter login"
          onChange={(e) => setLogin(e.target.value)}
          onKeyDown={onLoginKeyupHandler}
          autoFocus
        />
        <InputPassword
          inputRef={passwordRef}
          className="sign-in__pass-input"
          placeholder="Enter password"
          onChange={(e) => setPassword(e.target.value)}
          onKeyDown={onPasswordKeyupHandler}
        />
        <p className="sign-in__error form__error">{err}</p>
        <Button
          className="button"
          variant="contained"
          onClick={onLoginHandler}
          disabled={login.length === 0 || password.length === 0}
        >
          Login
        </Button>

        <NavLink
          className="sign-in__register"
          to="/signup"

        >
          Don't have account? Register!
        </NavLink>
      </Form>
    </div>
  );
};

export default SignIn;
