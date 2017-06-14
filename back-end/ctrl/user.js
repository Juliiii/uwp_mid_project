module.exports = (db) => {
  const user = db.collection('users');
  let record = require('./record.js')(db);
  function add (json) {
    return user.insert(json).then(res => Promise.resolve());
  }

  function duplimate (json) {
    return user.findOne({username: json.username})
               .then(res => Promise.resolve(!!res));
  }

  async function editAvatar (json) {
      user.update({"username" : json.username}, {$set: {"avatar" : json.avatar}});
      await record.editAvatar(json);
      return Promise.resolve();
  }

  async function editNickname (json) {
      user.update({"username" : json.username}, {$set: {"nickname" : json.nickname}});
      await record.editNickname(json);
      return Promise.resolve();
  }

  function findOne (json) {
    return user.findOne({username: json.username}).then(res => Promise.resolve(res));
  }

  return {
    add,
    duplimate,
    editAvatar,
    editNickname,
    findOne
  }
};