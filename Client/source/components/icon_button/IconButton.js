import React from 'react';

import { IconButton as BaseIconButton, createMuiTheme, withStyles, ThemeProvider } from '@material-ui/core';
import * as Colors from '@material-ui/core/colors';

import classNames from 'classnames';

import './IconButton.scss';

const customTheme = createMuiTheme({
    palette: {
        primary: Colors.amber,
        secondary: { main: '#c5cae9' }
    }
});

const IconButton = ({className, ...props}) => {
    return (
        <ThemeProvider theme={customTheme}>
            <BaseIconButton color="secondary" className={classNames("icon-btn", className)} {...props}>
            </BaseIconButton>
        </ThemeProvider>
    )
}

export default IconButton;