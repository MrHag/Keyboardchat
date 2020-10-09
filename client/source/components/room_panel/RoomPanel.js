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

    const joinRoom = (name) => {
        Socket.emit('joinroom', {
            name: name,
            password: null
        });
    }

    useEffect(() => {
        Socket.on('getrooms', data => {
            const rooms = data.data.map(room => {
                return { name: room.room }
            });
            setRooms(rooms);
        });

        Socket.on('roomchange', data => {
            Socket.emit('getrooms', { room: null});
        });

        Socket.on('joinroom', (data) => {
            console.error("Joinroom in RoomPanel unhandled!");
            if (data.successful) {
            }
        });

        Socket.emit('getrooms', {room: null});
        return () => { 
            Socket.off("getrooms");
            Socket.off('joinroom');
        }
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
                {rooms.map((room, index) => <RoomItem key={room.name + index} name={room.name} onRoomJoin={joinRoom}></RoomItem>)}
            </div>
        </div>
    )
}

export default RoomPanel;