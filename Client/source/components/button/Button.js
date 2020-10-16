import React from 'react';

import { Button as BaseButton } from '@material-ui/core';
import { withStyles, createMuiTheme, ThemeProvider } from '@material-ui/core';
import classNames from 'classnames';

import * as Colors from "@material-ui/core/colors";

import './Button.scss';

const customTheme = createMuiTheme({
    palette: {
        primary: Colors.brown,
        secondary: { main: '#c5cae9' },
    }
});

const styles = {
    root: {
        '&$disabled': {
            boxShadow: '0px 3px 1px -2px rgba(0,0,0,0.2), 0px 2px 2px 0px rgba(0,0,0,0.14), 0px 1px 5px 0px rgba(0,0,0,0.12)'
        },
    },
    disabled: {}
};

const Button = (props) => {
    const disabled = props.disabled;
    return (
        <ThemeProvider theme={customTheme}>
            <BaseButton className={classNames("button", {"disabled": disabled})} variant="contained" {...props}
            color={classNames({'secondary': !disabled, 'primary': disabled})}></BaseButton>
        </ThemeProvider>
    )
}

export default withStyles(styles)(Button);