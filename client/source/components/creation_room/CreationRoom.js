import React from 'react';
import PropTypes from 'prop-types';

import { Button } from '../index';
import { TextField } from '@material-ui/core';

import './CreationRoom.scss';

const CreationRoom = ({onComplete}) => {
    return (
        <div className="creation-room">
            <div className="creation-room__bg"></div>
            <div className="creation-room__content">
                <h2 className="creation-room__header">Create room</h2>
                <TextField className="creation-room__input" placeholder="Room name"></TextField>
                <TextField className="creation-room__input" placeholder="Room password"></TextField>
                <div className="creation-room__buttons">
                    <Button onClick={onComplete}>Cancel</Button>
                    <Button onClick={onComplete}>Create</Button>
                </div>
            </div>
        </div>
    )
}

CreationRoom.propTypes = {
    onComplete: PropTypes.func.isRequired,
}

export default CreationRoom;