import React, { useEffect, useRef, useState } from 'react';
import PropTypes from 'prop-types';

import { InputAdornment, TextField} from '@material-ui/core';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { RoomItem, Button, IconButton } from 'components';
import { Socket } from 'logic';

import './RoomPanel.scss';

try {
    var fake_rooms = require('fake_data/fake.json').rooms;
} catch (err) { }

const RoomList = ({ inRoom, rooms, joinRoom, leaveRoom }) => {
    const listRef = useRef();

    return (
        <div ref={listRef} className="room-panel__list">
            {
                rooms.map((room, index) => {
                    const isActive = room.name === inRoom;
                    return (
                        <RoomItem
                            key={room.name + index}
                            roomData={room} active={isActive}
                            onRoomJoin={joinRoom}
                            onRoomLeave={() => leaveRoom(room.name)}
                        />
                    )
                })
            }
        </div>
    );
}

const RoomPanel = ({ onCreateRoom }) => {
    const [rooms, setRooms] = useState((fake_rooms) ? fake_rooms : []);
    const [inRoom, setInRoom] = useState('global');
    const [searchQuery, setSearchQuery] = useState('');

    const joinRoom = (name, password) => {
        Socket.emit('joinroom', {
            name: name,
            password: password
        });
    };

    const leaveRoom = (name) => {
        console.log("Leaving room... Name = ", name);
        Socket.emit('leaveroom', {
            name: name
        });
    };

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
        if (data.successful) {
            setInRoom(data.data.room);
        }
    }

    const socketLeaveroom = (data) => {
        console.log("socketLeaveroom data = ", data);
        setInRoom('global');
    }

    const initSocketsListeners = () => {
        Socket.on('getrooms', socketGetrooms);
        Socket.on('roomchange', socketRoomchange);
        Socket.on('joinroom', socketJoinroom);
        Socket.on('leaveroom', socketLeaveroom)
        //Socket.emit('getrooms', {room: null});
    }

    const removeSocketListeners = () => {
        Socket.removeEventListener("getrooms", socketGetrooms);
        Socket.removeEventListener('joinroom', socketJoinroom);
        Socket.removeEventListener('roomchange', socketRoomchange);
        Socket.removeEventListener('leaveroom', socketLeaveroom);
    }

    useEffect(() => {
        initSocketsListeners();
        return removeSocketListeners;
    }, [])

    const onClearSearchBtn = () => {
        setSearchQuery('');
        Socket.emit('getrooms', {room: null});
    }

    const onSearchChange = (e) => {
        const name = (e.target.value !== '') ? e.target.value : null;
        Socket.emit('getrooms', {room: name});
        setSearchQuery(e.target.value);
    }

    return (
        <div className="room-panel">
            <TextField className="room-panel__search" placeholder="Type room name" value={searchQuery} onChange={onSearchChange}
                InputProps={{
                    endAdornment: (
                        <InputAdornment position="start">
                            <IconButton disabled={searchQuery===''} onClick={onClearSearchBtn} size="small">
                                <FontAwesomeIcon icon={FontAwesomeIcons.faTrash}></FontAwesomeIcon>
                            </IconButton>
                        </InputAdornment>
                    )
                }}
            />
            <RoomList inRoom={inRoom} rooms={rooms} joinRoom={joinRoom} leaveRoom={leaveRoom}></RoomList>
            <Button onClick={onCreateRoom}>Create room</Button>
        </div>
    )
}

RoomPanel.propTypes = {
    onCreateRoom: PropTypes.func.isRequired
}

export default RoomPanel;