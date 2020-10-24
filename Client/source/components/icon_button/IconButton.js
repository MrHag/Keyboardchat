import React from 'react';

import { IconButton as BaseIconButton, createMuiTheme, MuiThemeProvider } from '@material-ui/core';
import * as Colors from '@material-ui/core/colors';
import { makeStyles } from '@material-ui/styles';

import classNames from 'classnames';

import './IconButton.scss';

const customTheme = createMuiTheme({
    palette: {
        primary: Colors.amber,
        secondary: { main: '#c5cae9' },
        dark: {
            color: '#000',
        },
    }
});

const useStyles = makeStyles({
    root: {
        padding: 0,
        borderRadius: '50%',
    },
});

const IconButton = ({className, color, ...props}) => {
    const style = (color) ? customTheme.palette[color] : customTheme.palette.secondary;
    const classes = useStyles();
    return (
        <MuiThemeProvider theme={customTheme}>
            <BaseIconButton
                style={style}
                className={classNames("icon-btn", className)}
                classes={{
                    root: classes.root
                }}
                {...props}
            />
        </MuiThemeProvider>
    )
}

export default IconButton;