import React from 'react';

import { IconButton as BaseIconButton, createMuiTheme, withStyles, ThemeProvider, MuiThemeProvider } from '@material-ui/core';
import * as Colors from '@material-ui/core/colors';

import classNames from 'classnames';

import './IconButton.scss';

const customTheme = createMuiTheme({
    palette: {
        primary: Colors.amber,
        secondary: { main: '#c5cae9' },
        dark: {
            //backgroundColor: '#',
            color: '#000',
        },
    }
});

const IconButton = ({className, color, ...props}) => {
    const style = (color) ? customTheme.palette[color] : customTheme.palette.secondary;
    return (
        <MuiThemeProvider theme={customTheme}>
            <BaseIconButton color="secondary" style={style} className={classNames("icon-btn", className)} {...props}>
            </BaseIconButton>
        </MuiThemeProvider>
    )
}

export default IconButton;