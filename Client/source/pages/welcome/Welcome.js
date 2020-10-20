import React, { useState, useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import { Input} from '@material-ui/core';

import { Socket } from 'logic';
import { Button } from 'components';

import './Welcome.scss';

const Login = ({onLogin}) => {
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
            onLogin();
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
        <div className="login">
            <Input
                className="login__input"
                placeholder="Enter name"
                onChange={e => setLogin(e.target.value)}
                onKeyDown={onLoginKeyupHandler} 
                autoFocus={true} 
            />
            <Input inputRef={passwordRef}
                className="login__input"
                placeholder="Enter password"
                onChange={e => setPassword(e.target.value)}
                onKeyDown={onPasswordKeyupHandler} 
            />
            <p className="login__error">{err}</p>
            <Button
                className="button"
                variant="contained"
                color="secondary"
                onClick={onLoginHandler}
                disabled={login.length === 0 || password.length === 0}
            >
                Login
            </Button>
        </div>
    )
}

Login.propTypes = {
    onLogin: PropTypes.func.isRequired
};

const Welcome = ({onLogin}) => {
    useEffect(() => {
        Socket.on('responce', (data) => {
            if (data.type == 'authSucc') {
                onLogin();
            }
        })
    });

    return (
        <div className="welcome">
            <Login onLogin={onLogin}></Login>
        </div>
    )
}

Welcome.propTypes = {
    onLogin: PropTypes.func.isRequired
}

export default Welcome;