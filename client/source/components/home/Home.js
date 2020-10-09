import React, { useState, useEffect } from 'react';
import { Chat, RoomPanel } from '../index';

import './Home.scss';

import Socket from '../../Socket';

const Home = () => {
    return (
        <div className="home">
            <RoomPanel></RoomPanel>
            <Chat></Chat>
        </div>
    )
}

export default Home;