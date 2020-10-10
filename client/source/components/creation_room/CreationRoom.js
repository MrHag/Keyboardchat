import React, { useState } from 'react';
import PropTypes from 'prop-types';

import { Button } from '../index';
import { TextField } from '@material-ui/core';

import Socket from '../../Socket';

import './CreationRoom.scss';

const CreationRoom = ({onComplete}) => {
    const [name, setName] = useState('');
    const [password, setPassword] = useState('');

    /*
        Пока joinroom всегда возвращает success. Если это изменится, то нужно будет хендлить ошибки здесь
    */

    const onCreateBtn = () => {
        Socket.emit('joinroom', {
            name: name,
            password: (password.length === 0) ? null : password
        });
        onComplete();
    }

    return (
        <div className="creation-room">
            <div className="creation-room__bg"></div>
            <div className="creation-room__content">
                <h2 className="creation-room__header">Create room</h2>
                <TextField onChange={e => setName(e.target.value)} className="creation-room__input" placeholder="Room name"></TextField>
                <TextField onChange={e => setPassword(e.target.value)} className="creation-room__input" placeholder="Room password"></TextField>
                <div className="creation-room__buttons">
                    <Button onClick={onComplete}>Cancel</Button>
                    <Button onClick={onCreateBtn}>Create</Button>
                </div>
            </div>
        </div>
    )
}

CreationRoom.propTypes = {
    onComplete: PropTypes.func.isRequired,
}

export default CreationRoom;