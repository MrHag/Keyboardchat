const webpack = require('webpack');
const path = require('path');
const htmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

const isDevelopment = /development/.test(process.env.NODE_ENV);

function CreateTemplateHTML(app_name)
{
    return (
`<!DOCTYPE html>
<html lang="en">
    <head>
        <meta charset="utf-8">
        <meta content="ie=edge" http-equiv="x-ua-compatible">
        <meta name="viewport" content="width=device-width, height=device-height initial-scale=1.0">
        <title>${app_name}</title>
    </head>
    <body>
        <div id="root"></div>
    </body>
</html>`
    )
}

const APP_NAME = 'Keyboardchat';

function includePlugins() {
    let plugins = [
        new htmlWebpackPlugin({
            templateContent: CreateTemplateHTML(APP_NAME)
        }),
        new MiniCssExtractPlugin({
            filename: "bundle.css"
        })
    ];
    //Don't include fake data if you're building production version
    if (!isDevelopment) {
        console.log("Ignore fake.json!");
        plugins.push(
            new webpack.IgnorePlugin({
                resourceRegExp: /fake\.json$/,
            })
        )
    }

    return plugins;
}

function generateConfig(output_path) {
    return {
        entry: [path.resolve(__dirname, "./source/index.js")],

        resolve: {
            extensions: ['.js', '.jsx'],
            alias: {
                'react-dom': '@hot-loader/react-dom'
            },
        },

        module: {
            rules: [
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    loader: 'babel-loader',
                    options: {
                        presets: ["@babel/preset-react"],
                        plugins: ["react-hot-loader/babel"],
                    }
                },
                {
                    test: /\.s[ac]ss$/i,
                    use: [
                        MiniCssExtractPlugin.loader,
                        'css-loader',
                        'sass-loader',
                    ],
                }
            ]
        },

        devtool: 'eval-source-map',
        
        devServer: {
            contentBase: output_path,
            port: 3000,
            hot: true,
            proxy: {
                '/': 'http://localhost:5000' //Allows to make request from React.app to server on 4000
            }
        },

        plugins: includePlugins(),
        output: {
            path: path.resolve(__dirname, output_path),
            filename: "bundle.js"
        }
    }
}

function makeOutputPath() {
    //return path.resolve(__dirname, `./server/WEB pages/${process.env.NODE_ENV}/`);
    return path.resolve(__dirname, `../public/`);
}

module.exports = generateConfig(makeOutputPath());