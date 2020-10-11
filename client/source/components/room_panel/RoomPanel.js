import React, { useEffect, useRef, useState } from 'react';
import PropTypes from 'prop-types';

import './RoomPanel.scss';
import { InputAdornment, TextField, IconButton} from '@material-ui/core';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';
import { RoomItem, Button } from '../../components';

import Socket from '../../Socket';

try {
    var fake_rooms = require('../../../fake_data/fake.json').rooms;
} catch (err) { }

const RoomList = ({ inRoom, rooms, joinRoom }) => {
    const listRef = useRef();

    return (
        <div ref={listRef} className="room-panel__list">
            {
                rooms.map((room, index) => {
                    const isActive = room.name === inRoom;
                    return <RoomItem key={room.name + index} roomData={room} active={isActive} onRoomJoin={joinRoom}></RoomItem>
                })
            }
        </div>
    )
}

const RoomPanel = ({ onCreateRoom }) => {
    const [rooms, setRooms] = useState((fake_rooms) ? fake_rooms : []);
    const [inRoom, setInRoom] = useState('global');
    const [searchQuery, setSearchQuery] = useState('');

    const joinRoom = (name, password) => {
        console.log("Trying to join room in RoomPanel: ");
        console.log("name = ", name);
        console.log("password = ", password);
        Socket.emit('joinroom', {
            name: name,
            password: password
        });
    }

    const socketGetrooms = (data) => {
        const rooms = data.data.map(room => {
            return { name: room.room, haspass: room.haspass }
        });
        setRooms(rooms);
    }

    const socketRoomchange = (data) => {
        Socket.emit('getrooms', { room: null});
    }

    const socketJoinroom = (data) => {
        console.error("Hello world!");
        if (data.successful) {
            console.error("New room name = ", data.data.room);
            setInRoom(data.data.room);
        }
    }

    const initSocketsListeners = () => {
        Socket.on('getrooms', socketGetrooms);
        Socket.on('roomchange', socketRoomchange);
        Socket.on('joinroom', socketJoinroom);
        Socket.emit('getrooms', {room: null});
    }

    const removeSocketListeners = () => {
        Socket.removeEventListener("getrooms", socketGetrooms);
        Socket.removeEventListener('joinroom', socketJoinroom);
        Socket.removeEventListener('roomchange', socketRoomchange);
    }

    useEffect(() => {
        initSocketsListeners();
        return removeSocketListeners;
    }, [])

    const onSearchBtn = () => {
        const name = (searchQuery !== '') ? searchQuery : null;
        Socket.emit('getrooms', {room: name});
    }

    return (
        <div className="room-panel">
            <TextField className="room-panel__search" placeholder="Type room name" value={searchQuery} onChange={e => setSearchQuery(e.target.value)}
                InputProps={{
                    endAdornment: (
                        <InputAdornment position="start">
                            <IconButton onClick={onSearchBtn} size="small"><FontAwesomeIcon icon={FontAwesomeIcons.faSearch}></FontAwesomeIcon></IconButton>
                        </InputAdornment>
                    )
                }}
            />
            <RoomList inRoom={inRoom} rooms={rooms} joinRoom={joinRoom}></RoomList>
            <Button onClick={onCreateRoom}>Create room</Button>
        </div>
    )
}

RoomPanel.propTypes = {
    onCreateRoom: PropTypes.func.isRequired
}

export default RoomPanel;