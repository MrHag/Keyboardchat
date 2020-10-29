import React, { useState, useRef, useEffect } from 'react';
import { useHistory, NavLink } from 'react-router-dom';

import { Socket } from 'logic';
import { Button, Input, InputPassword, Form } from 'components';

import './SignUp.scss';

const SignUp = () => {
  const routeHistory = useHistory();

  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [login, setLogin] = useState('');
  const [err, setErr] = useState('');

  const passwordRef = useRef();

  const onSignUpHandler = () => {
    if (confirm === password) {
      const request = {
        name: login,
        password,
      };
      Socket.emit('registration', request);
    } else {
      setErr('Confirm and your password don\'t match!');
    }
  };

  const socketRegistration = (data) => {
    console.log('Auth response!');
    console.log('Response data = ', data);
    if (data.successful) {
      routeHistory.push('/');
    } else {
      switch (data.data) {
        case 'nameExists':
          setErr('This name is already taken');
          break;
        default:
          break;
      }
    }
  };

  useEffect(() => {
    Socket.on('registration', socketRegistration);
    return () => Socket.removeEventListener('registration', socketRegistration);
  }, []);

  const onLoginKeyupHandler = (e) => {
    if (e.key === 'Enter') {
      console.log('Login enter up...');
      passwordRef.current.focus();
    }
  };

  const onPasswordKeyupHandler = (e) => {
    if (e.key === 'Enter') {
      onSignUpHandler(e);
    }
  };

  return (
    <div className="sign-up">
      <Form
        name="SignUp"
        error={err}
      >
        <Input
          className="sign-up__login-input"
          placeholder="Enter login"
          onChange={(e) => setLogin(e.target.value)}
          onKeyDown={onLoginKeyupHandler}
          autoFocus
        />
        <div className="group">
          <InputPassword
            inputRef={passwordRef}
            className="sign-up__pass-input"
            placeholder="Enter password"
            onChange={(e) => setPassword(e.target.value)}
            onKeyDown={onPasswordKeyupHandler}
          />
          <InputPassword
            className="sign-up__confirm-input"
            placeholder="Confirm password"
            onChange={(e) => setConfirm(e.target.value)}
            onKeyDown={onPasswordKeyupHandler}
          />
        </div>
        <p className="sign-up__error form__error">{err}</p>
        <Button
          className="button"
          variant="contained"
          onClick={onSignUpHandler}
          disabled={login.length === 0 || password.length === 0}
        >
          Signup
        </Button>

        <NavLink
          className="sign-up__register"
          to="/"
        >
          Already have account? Sigin!
        </NavLink>
      </Form>
    </div>
  );
};

export default SignUp;
