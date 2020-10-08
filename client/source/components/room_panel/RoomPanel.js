import React, { useState } from 'react';

import './RoomPanel.scss';
import { InputAdornment, TextField, IconButton } from '@material-ui/core';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import * as FontAwesomeIcons from '@fortawesome/free-solid-svg-icons';

const RoomItem = ({name}) => {
    return (
        <div className="room-item">
            <div className="room-item__name">{name}</div>
        </div>
    )
}

const fake_rooms = require('../../../fake_data/fake.json').rooms;

const RoomPanel = () => {
    const [rooms, setRooms] = useState((fake_rooms) ? fake_rooms : []);

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
                {rooms.map((room, index) => <RoomItem key={index} name={room.name}></RoomItem>)}
            </div>
        </div>
    )
}

export default RoomPanel;