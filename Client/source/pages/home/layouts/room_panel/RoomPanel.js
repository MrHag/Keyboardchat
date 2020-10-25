import React, { useEffect, useRef, useState, useContext } from 'react';
import PropTypes from 'prop-types';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

import { Input, RoomItem, Button, IconButton } from 'components';
import { Socket, UserData } from 'logic';

import './RoomPanel.scss';

try {
    var fake_rooms = require('fake_data/fake.json').rooms;
} catch (err) { }

const RoomList = ({ inRoom, rooms, joinRoom, leaveRoom }) => {
    const listRef = useRef();
    console.log("InRoom = ", inRoom);
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

const RoomPanel = ({ onCreateRoom, onRoomLeave }) => {
    const [rooms, setRooms] = useState((fake_rooms) ? fake_rooms : []);
    const [searchQuery, setSearchQuery] = useState('');
    const [inRoom, setInRoom] = useState(UserData.inRoom);

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
        onRoomLeave();
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
            UserData.inRoom = data.data.room;
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
            <div className="room-panel__search">
                <Input
                    className="room-panel__search"
                    placeholder="Room search"
                    value={searchQuery}
                    onChange={onSearchChange}
                    centered
                />
                <IconButton 
                    disabled={searchQuery === ''}
                    onClick={onClearSearchBtn}
                    size="small"
                >
                    <FontAwesomeIcon icon={FontAwesomeIcons.faTrash}></FontAwesomeIcon>
                </IconButton>
            </div>
            <RoomList inRoom={UserData.inRoom} rooms={rooms} joinRoom={joinRoom} leaveRoom={leaveRoom}></RoomList>
            <Button onClick={onCreateRoom}>Create room</Button>
        </div>
    )
}

RoomPanel.propTypes = {
    onCreateRoom: PropTypes.func.isRequired
}

export default RoomPanel;