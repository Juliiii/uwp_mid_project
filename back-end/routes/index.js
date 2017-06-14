var express = require('express');
var router = express.Router();


function f(db) {
  const records = require('../ctrl/record.js')(db);
  const user = require('../ctrl/user.js')(db);
  const comment = require('../ctrl/comment.js')(db);

  function auth (req, res, next) {
    if (typeof req.cookies.username != 'undefined') {
      return next();
    }
    res.send(JSON.stringify({status: 401}));
  }

  router.post('/session', async (req, res, next) => {
    const one = await user.findOne(req.body);
    let status,
        message;
    if (!one) {
      status = 404;
      message = '用户名不存在';
    } else {
      status = req.body.password === one.password ? 200 : 404;
      message = req.body.password === one.password ? "欢迎回来" : "密码错误";
    }

    if (status === 200) {
      res.cookie("username", req.body.username, {maxAge: 60000 * 60 * 24, httpOnly: true});
    }

    const result = {
      status,
      message
    }
    res.send(JSON.stringify(result));
  });

  router.post('/user', async (req, res, next) => {
    const exist = await user.duplimate(req.body);
    let result;
    if (exist) {
      result = {
        status: 404,
        message: '用户已存在'
      };
    } else {
      await user.add(req.body);
      result = {
        status: 200,
        message: '注册成功'
      };
    }
    res.send( JSON.stringify(result) );
  });

  router.get('/user', auth, async (req, res, next) => {
    const one = await user.findOne({username: req.cookies.username});
    res.send(JSON.stringify(one));
  });

  router.get('/logout', auth, (req, res, next) => {
    res.clearCookie('username');
    res.send(JSON.stringify({status: 200, message: "退出成功"}));    
  });


  router.post('/editNickname', auth, async (req, res, next) => {
    req.body.username = req.cookies.username;
    await user.editNickname(req.body);
    await comment.editNickname(req.body);
    res.send(JSON.stringify({status: 200, message: "修改成功"}));
  });

  router.post('/editAvatar', async (req, res, next) => {
    req.body.username = req.cookies.username;
    await user.editAvatar(req.body);
    res.send(JSON.stringify({status: 200, message: "修改成功"}));
  });


  router.get('/dakas', auth, async (req, res, next) => {
    const data = await records.all();
    res.send( JSON.stringify({data, length: data.length}) )
  });

  router.get('/records', auth, async (req, res, next) => {
    const data = await records.mine({username: req.cookies.username});
    res.send ( JSON.stringify({data, length:data.length}) );  
  });

  router.post('/records', auth, async (req, res, next) => {
    await records.add(req.body);
    res.send ( JSON.stringify({status: 200, message: "添加成功"}));
  });

  router.post('/comments', auth, async (req, res, next) => {
    req.body.username = req.cookies.username;
    await comment.add(req.body);
    res.send(JSON.stringify({status: 200, message: '评论成功'}));
  });

  router.get('/comments', async (req, res, next) => {
    req.body.id = req.query.id;
    const data = await comment.get(req.body);
    res.send(JSON.stringify({data, length: data.length}));
  });
  return router;
}


module.exports = f;
