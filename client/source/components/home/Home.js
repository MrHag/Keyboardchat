import React, { useState } from 'react';

import { Chat, RoomPanel } from '../index';

import './Home.scss';

const Home = () => {
    return (
        <div className="home">
            <RoomPanel></RoomPanel>
            <Chat></Chat>
        </div>
    )
}

export default Home;