const Webpack = require('webpack');
const path = require('path');
const HTMLWebpackPlugin = require('html-webpack-plugin');
const MiniSCCExtrractPlugin = require('mini-css-extract-plugin');
const ESLintPlugin = require('eslint-webpack-plugin');

const isDev = (/development/).test(process.env.NODE_ENV);
const outputPath = '../public/';

function generateModule() {
  return {
    rules: [
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: [
          {
            loader: 'babel-loader',
            options: {
              presets: ['@babel/preset-react'],
              plugins: ['react-hot-loader/babel'],
            },
          },
        ],
      },
      {
        test: /\.s[ac]ss$/i,
        use: [
          MiniSCCExtrractPlugin.loader,
          'css-loader',
          'sass-loader',
        ],
      },
    ],
  };
}

function generatePlugins() {
  const plugins = [
    new HTMLWebpackPlugin({
      template: './source/index.html',
      favicon: './favicon.ico',

    }),
    new MiniSCCExtrractPlugin({
      filename: 'bundle.css',
    }),
  ];

  if (!isDev) {
    plugins.push(
      new Webpack.IgnorePlugin({
        resourceRegExp: /fake\.json$/,
      }),
    );
  }

  if (isDev) {
    plugins.push(
      new ESLintPlugin({
        extensions: ['js', 'jsx'],
        emitWarning: true,
        failOnError: !isDev,
      }),
    );
  }

  return plugins;
}

function generateConfig() {
  return {
    resolve: {
      extensions: ['.js', '.jsx'],
      alias: {
        shared: path.resolve(__dirname, './source/shared/'),
        fake_data: path.resolve(__dirname, './fake_data/'),
        styles: path.resolve(__dirname, './source/styles/'),
        logic: path.resolve(__dirname, './source/logic/'),
        components: path.resolve(__dirname, './source/components/'),
        layouts: path.resolve(__dirname, './source/layouts/'),
      },
    },

    mode: isDev ? 'development' : 'production',

    entry: path.resolve(__dirname, './source/index.jsx'),

    module: generateModule(),

    plugins: generatePlugins(),

    devtool: isDev ? 'eval-source-map' : false,
    devServer: {
      // open: s,
      hot: true,
      contentBase: outputPath,
      port: 3000,
      proxy: {
        '/': 'http://localhost:5000',
      },
    },

    output: {
      path: path.resolve(__dirname, outputPath),
      filename: 'bundle.js',
    },
  };
}

module.exports = generateConfig();

/* const webpack = require('webpack');
const path = require('path');
const htmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

/*const isDevelopment = /development/.test(process.env.NODE_ENV);

const APP_NAME = 'Keyboardchat';

function includePlugins() {
    let plugins = [
        new htmlWebpackPlugin({
            template: './source/index.html',
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

module.exports = generateConfig(makeOutputPath()); */
