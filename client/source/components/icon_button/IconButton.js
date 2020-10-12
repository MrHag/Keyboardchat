import React from 'react';

import { IconButton as BaseIconButton, createMuiTheme, ThemeProvider } from '@material-ui/core';
import * as Colors from '@material-ui/core/colors';

import './IconButton.scss';

const customTheme = createMuiTheme({
    palette: {
        primary: Colors.amber,
        secondary: { main: '#c5cae9' }
    }
});

const IconButton = (props) => {
    return (
        <ThemeProvider theme={customTheme}>
            <BaseIconButton color="secondary" className="chat-input__send" {...props}>
            </BaseIconButton>
        </ThemeProvider>
    )
}

export default IconButton;