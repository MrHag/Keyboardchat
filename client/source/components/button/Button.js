import React from 'react';

import { Button as BaseButton } from '@material-ui/core';
import { createMuiTheme, ThemeProvider } from '@material-ui/core';

import * as Colors from "@material-ui/core/colors";

import './Button.scss';

const customTheme = createMuiTheme({
    palette: {
        primary: Colors.amber,
        secondary: { main: '#c5cae9' }
    }
});

const Button = (props) => {
    return (
        <ThemeProvider theme={customTheme}>
            <BaseButton className="button" color="secondary" variant="contained" {...props}></BaseButton>
        </ThemeProvider>
    )
}

export default Button;