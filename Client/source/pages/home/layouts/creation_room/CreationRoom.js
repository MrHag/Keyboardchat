import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';

import { Input, InputPassword, Button } from 'components';
import { Socket } from 'logic';

import './CreationRoom.scss';

const CreationRoom = ({onComplete}) => {
    const [name, setName] = useState('');
    const [password, setPassword] = useState('');
    const [err, setErr] = useState('');

    /*
        Пока joinroom всегда возвращает success. Если это изменится, то нужно будет хендлить ошибки здесь
    */

    const onCreateBtn = () => {
        Socket.emit('createroom', {
            name: name,
            password: (password.length === 0) ? null : password
        });
    }

    const socketCreateRoom = (data) => {
        console.log("Socket create room = ", data);
        if (data.successful) {
            onComplete();
        } else {
            //Here might be other types of errors
            setErr('Room with this name already exist!');
        }
    }

    useEffect(() => {
        Socket.on('createroom', socketCreateRoom)
        return () => Socket.removeEventListener('createroom', socketCreateRoom);
    }, [])

    const onInputKeydown = (e) => {
        if (e.key === 'Enter') {
            onCreateBtn();
        }
    }

    return (
        <div className="creation-room">
            <div className="creation-room__bg"></div>
            <div className="creation-room__content">
                <h2 className="creation-room__header">Create room</h2>
                <Input
                    onChange={e => setName(e.target.value)}
                    className="creation-room__input"
                    placeholder="Room name"
                    onKeyDown={onInputKeydown} autoFocus
                />
                <InputPassword
                    onChange={e => setPassword(e.target.value)}
                    className="creation-room__input"
                    placeholder="Room password"
                    onKeyDown={onInputKeydown}
                />
                <p className="creation-room__error">{err}</p>
                <div className="creation-room__buttons">
                    <Button onClick={onComplete}>Cancel</Button>
                    <Button disabled={name === ''} onClick={onCreateBtn}>Create</Button>
                </div>
            </div>
        </div>
    )
}

CreationRoom.propTypes = {
    onComplete: PropTypes.func.isRequired,
}

export default CreationRoom;