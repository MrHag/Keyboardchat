import React, { useState, useRef, useEffect } from 'react';
import { useHistory, NavLink } from 'react-router-dom';

import { Socket } from 'logic';
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
    const request = {
      name: login,
      password,
    };
    console.log('Socket.emit("auth", ...)');
    Socket.emit('auth', request);
  };

  const socketAuth = (data) => {
    console.log('Auth response!');
    console.log('Response data = ', data);
    if (data.successful) {
      routeHistory.push(ROUTES.Home.route);
    } else {
      switch (data.data) {
        case 'badName':
          setErr('Bad name!');
          break;
        case 'invalidData':
          setErr('Invalid data!');
          break;
        default:
          break;
      }
    }
  };

  useEffect(() => {
    Socket.on('auth', socketAuth);
    return () => Socket.removeEventListener('auth', socketAuth);
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
