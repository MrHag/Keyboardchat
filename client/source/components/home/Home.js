import React, { useState, useEffect } from 'react';
import { Chat, RoomPanel } from '../index';

import './Home.scss';

const Home = () => {
    const onCreateRoom = () => {
        alert("Create room!");
    }

    return (
        <div className="home">
            <RoomPanel onCreateRoom={onCreateRoom}></RoomPanel>
            <Chat></Chat>
        </div>
    )
}

export default Home;