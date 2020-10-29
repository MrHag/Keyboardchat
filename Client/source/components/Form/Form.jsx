import React from 'react';

import './Form.scss';

const Form = ({ name, ...props }) => (
  <div className="form">
    <h2 className="form__name">{name}</h2>
    {props.children}
  </div>
);

export default Form;
