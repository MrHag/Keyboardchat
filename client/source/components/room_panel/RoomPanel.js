import React, { useEffect, useState } from 'react';

import './RoomPanel.scss';
import { InputAdornment, TextField, IconButton } from '@material-ui/core';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';
import Socket from '../../Socket';
import { RoomItem } from '../../components';

try {
    var fake_rooms = require('../../../fake_data/fake.json').rooms;
} catch (err) { }

const RoomPanel =  () => {
    const [rooms, setRooms] = useState((fake_rooms) ? fake_rooms : []);
    const [inRoom, setInRoom] = useState('global');

    const joinRoom = (name) => {
        Socket.emit('joinroom', {
            name: name,
            password: null
        });
    }

    const socketGetrooms = (data) => {
        const rooms = data.data.map(room => {
            return { name: room.room }
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

    return (
        <div className="room-panel">
            <TextField className="room-panel__search" placeholder="Type room name"
                InputProps={{
                    endAdornment: (
                        <InputAdornment position="start">
                            <IconButton size="small"><FontAwesomeIcon icon={FontAwesomeIcons.faSearch}></FontAwesomeIcon></IconButton>
                        </InputAdornment>
                    )
                }}
            />
            <div className="room-panel__list">
                {
                    rooms.map((room, index) => {
                        const isActive = room.name === inRoom;
                        console.log("IN room = ", inRoom);
                        if (isActive) {
                            console.error("IsActive room! ", room.name);
                        }
                        return <RoomItem key={room.name + index} name={room.name} active={isActive} onRoomJoin={joinRoom}></RoomItem>
                    })
                }
            </div>
        </div>
    )
}

export default RoomPanel;