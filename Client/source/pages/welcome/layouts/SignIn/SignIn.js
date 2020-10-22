import React, { useState, useRef, useEffect } from 'react';
import { useHistory, NavLink } from 'react-router-dom';

import { Socket } from 'logic';
import { Button, Input, InputPassword } from 'components';

import './SignIn.scss';

const SignIn = () => {
  const routeHistory = useHistory();

  const [password, setPassword] = useState('');
  const [login, setLogin] = useState('');
  const [err, setErr] = useState('');

  const passwordRef = useRef();

  const onLoginHandler = () => {
      let request = {
          name: login,
          password: password,
      };
      Socket.emit('auth', request);
  }

  const socketAuth = (data) => {
      console.log("Auth response!");
      console.log("Response data = ", data);
      if (data.successful) {
          routeHistory.push('/chat');
      } else {
          switch (data.data) {
              case 'badName':
                  setErr('Bad name!');
                  break;
          }
      }
  }

  useEffect(() => {
      Socket.on('auth', socketAuth);
      return () => Socket.removeEventListener('response', socketAuth);
  }, [])

  const onLoginKeyupHandler = (e) => {
      if (e.key === "Enter") {
          console.log("Login enter up...");
          passwordRef.current.focus();
      }
  }

  const onPasswordKeyupHandler = (e) => {
      if (e.key === 'Enter') {
          onLoginHandler(e);
      }
  }

  return (
      <div className="sign-in">
        <h1>Sign in</h1>
        <Input
            className="sign-in__login-input"
            placeholder="Enter login"
            onChange={e => setLogin(e.target.value)}
            onKeyDown={onLoginKeyupHandler} 
            autoFocus={true} 
        />
        <InputPassword inputRef={passwordRef}
            className="sign-in__pass-input"
            placeholder="Enter password"
            onChange={e => setPassword(e.target.value)}
            onKeyDown={onPasswordKeyupHandler} 
        />
        <p className="sign-in__error">{err}</p>
        <Button
            className="button"
            variant="contained"
            color="secondary"
            onClick={onLoginHandler}
            disabled={login.length === 0 || password.length === 0}
        >
            Login
        </Button>

        <NavLink
            className="sign-in__register"
            to='/signup'
        >
            Don't have account? Register!
        </NavLink>
      </div>
  )
}

export default SignIn;