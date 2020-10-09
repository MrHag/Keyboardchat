import React, { useEffect } from 'react';
import PropTypes from 'prop-types';

import Socket from '../../Socket';

import './RoomItem.scss';

const RoomItem = ({name, onRoomJoin}) => {
    return (
        <div onClick={e => onRoomJoin(name)} className="room-item">
            <div className="room-item__name">{name}</div>
        </div>
    )
}

RoomItem.propType = {
    name: PropTypes.string.isRequired,
    onRoomJoin: PropTypes.func.isRequired
}

export default RoomItem;
