import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { Input} from '@material-ui/core';

import './Welcome.scss';
import Socket from '../../Socket';
import { Button } from '../index';

const Login = ({onLogin}) => {
    const [login, setLogin] = useState('');
    const [err, setErr] = useState('');

    const onLoginHandler = () => {
        let request = {
            name: login
        };
        Socket.emit('auth', request);
    }

    const socketAuth = (data) => {
        console.log("Auth response!");
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

    return (
        <div className="login">
            <Input className="login__login" placeholder="Enter name" onChange={e => setLogin(e.target.value)} 
                onKeyDown={e => {
                    if (e.key == 'Enter')
                        onLoginHandler(e)
                }} autoFocus={true}></Input>
            <p className="login__error">{err}</p>
            <Button className="button" variant="contained" color="secondary" onClick={onLoginHandler} disabled={login.length === 0}>Login</Button>
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