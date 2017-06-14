module.exports = (db) => {
  const records = db.collection('records');
  // let record = require('./record.js')(db);
  function add (json) {
    records.insert(json);
    return Promise.resolve();
  }

  function all (json) {
    return records.find({})
                  .sort({"createdTime":-1})
                  .toArray()
                  .then(res => Promise.resolve(res));
  }

  function mine (json) {
    return records.find({username: json.username})
              .sort({"createdTime":-1})
              .toArray()
              .then(res => Promise.resolve(res));
  }

  function editAvatar (json) {
    records.find({"username": json.username}).toArray().then(res => console.log(res));
    records.update({"username": json.username}, {$set: {"avatarUri": json.avatar}}, {upsert: false, multi: true});
    return Promise.resolve();
  }

  function editNickname (json) {
    records.update({"username": json.username}, {$set: {"nickname": json.nickname}}, {upsert: false, multi: true});
    return Promise.resolve();
  }
  return {
    add,
    all,
    mine,
    editAvatar,
    editNickname
  }
};