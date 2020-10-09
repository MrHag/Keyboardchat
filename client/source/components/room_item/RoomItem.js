import React, { useEffect } from 'react';
import PropTypes from 'prop-types';

import Socket from '../../Socket';

import './RoomItem.scss';

const RoomItem = ({name}) => {
    const joinRoom = () => {
        Socket.emit('JoinRoom', {
            name: name
        });
    }

    useEffect(() => {
        Socket.on('response', (data) => {
            console.log("On joint response = ", data);
        });
        return () => Socket.off('response');
    }, []);

    return (
        <div className="room-item">
            <div className="room-item__name" onClick={_ => joinRoom()}>{name}</div>
        </div>
    )
}

RoomItem.propType = {
    name: PropTypes.string.isRequired
}

export default RoomItem;
